class_name Chunk extends HierachicalPathfinder

var _data : Array
var _pathfinding : AStar3D = AStar3D.new()
var _chunk_manager : ChunkManager = null
var _entrances : Dictionary[int, Entrance] = {}

var dimensions : Vector3i
var position : Vector3i
var full_chunk_lock : ReadWriteLock = ReadWriteLock.new()
var path_finding_filter : Callable = self._default_path_finding_filter
var edges : Dictionary = {}

func _init(position : Vector3i, dimensions : Vector3i):
	self.position = position
	self.dimensions = dimensions
	_init_data(dimensions)

func _init_data(dimensions : Vector3i):
	for x in range(dimensions.x):
		_data.append([])
		for y in range(dimensions.y):
			_data[x].append([])
			for z in range(dimensions.z):
				_data[x][y].append(null)

func _is_on_edge(x : int, y : int, z : int) -> bool:
	if x == 0 || y == 0 || z == 0:
		return true
	
	if x == dimensions.x - 1 || y == dimensions.y - 1 || z == dimensions.z - 1:
		return true
	
	return false

func _isInBounds(x : int, y : int, z : int) -> bool:
	if x < 0 || y < 0 || z < 0:
		return false
	
	if x > dimensions.x - 1:
		return false
	if y > dimensions.y - 1:
		return false
	if z > dimensions.z - 1:
		return false
	
	return true

func _getVal(x : int, y : int, z :int) -> Holder:
	if !_isInBounds(x, y, z):
		return null
	
	return _data[x][y][z]

func _getValVect(pos : Vector3i) -> Holder:
	return _getVal(pos.x, pos.y, pos.z)

func _get_point_id(x : int, y : int, z : int) -> int:
	return x * (dimensions.y * dimensions.z) + y * dimensions.z + z

func _get_point_by_id(id : int) -> Vector3i:
	var x : int = id / (dimensions.y * dimensions.z)
	var rem = id % (dimensions.y * dimensions.z)
	var y : int = rem / dimensions.z
	var z = rem % dimensions.z
	return Vector3i(x, y, z)

# Only call from locked
func _add_point_to_pathfinding(x : int, y : int, z : int):
	var id = _get_point_id(x, y, z)
	_pathfinding.add_point(id, Vector3(x, y, z))
	if _is_on_edge(x, y, z):
		edges.set(Vector3i(x, y, z), false)

func _join_pathfinding_points():
	for id in _pathfinding.get_point_ids():
		var pos = _get_point_by_id(id)
		for offset in [[1, 0, 0], [0, 1, 0], [0, 0, 1], [1, 1, 0], [0, 1, 1], [1, -1, 0], [0, -1, 1]]:
			var otherId = _get_point_id(pos.x + offset[0], pos.y + offset[1], pos.z + offset[2])
			if _pathfinding.has_point(otherId):
				_pathfinding.connect_points(id, otherId, true)

func _default_path_finding_filter(x : int, y : int, z : int) -> bool:
	var val = _getVal(x, y, z)
	
	if val == null:
		var under = _getVal(x, y - 1, z)
		if under != null && under.is_walkable:
			return true
	else:
		if val.pass_through:
			return true
			
	return false

func _calculate_pathfinding():
	full_chunk_lock.write_lock()
	_pathfinding.clear()
	
	for x in range(dimensions.x):
		for y in range(dimensions.y):
			for z in range(dimensions.z):
				if path_finding_filter.call(x, y, z):
					_add_point_to_pathfinding(x, y, z)
	
	_join_pathfinding_points()
	_create_entrances()
	
	full_chunk_lock.write_unlock()

func _get_neighbors_pos_by_edge(edge : Vector3i) -> Array[Vector3i]:
	var results : Array[Vector3i] = []
	if edge.x == 0:
		results.append(Vector3i(-1, edge.y, edge.z))
	if edge.x == dimensions.x - 1:
		results.append(Vector3i(dimensions.x, edge.y, edge.z))
	if edge.z == 0:
		results.append(Vector3i(edge.x, edge.y, -1))
	if edge.z == dimensions.z - 1:
		results.append(Vector3i(edge.x, edge.y, dimensions.z))
	
	return results

func _create_entrances():
	# Check which edges have path in the neighboring chunk
	var valid_connections = {}
	for edge in edges.keys():
		for neighborEdge in _get_neighbors_pos_by_edge(edge):
			if _chunk_manager.try_global_has_edge(to_global(neighborEdge)):
				valid_connections.set(edge, [edge, neighborEdge])
	# Get the two edges and the middle of each consecutive entrance part
	var new_entrances : Dictionary[int, Entrance] = {}
	while !valid_connections.keys().is_empty():
		var startKey = valid_connections.keys().back()
		var currConnection = valid_connections.get(startKey)
		var together = []
		var dir = currConnection[1] - currConnection[0]
		var buffer = [currConnection]
		while !buffer.is_empty():
			currConnection = buffer.pop_back()
			together.append(currConnection)
			valid_connections.erase(currConnection[0])
			for from in valid_connections:
				if (currConnection[0] - from).length() <= 1.01:
					var connection = valid_connections.get(from)
					if connection[1] - connection[0] == dir:
						buffer.push_front(connection)
		together.sort_custom(func (a, b):
			return ChunkUtil.compare_vectors(a[0], b[0]))
		
		var chosen = []
		var idxs = [0, together.size() / 2, together.size() - 1]
		for i in range(min(3, together.size())):
			chosen.append(together[idxs[i]])
		
		for conn in chosen:
			var entrance = Entrance.new(conn[0], conn[1])
			new_entrances.set(entrance.get_id(), entrance)
	
	# These will be the actual entrances
	var entrance_ids_to_add = []
	for new in new_entrances:
		if _entrances.has(new):
			continue
		entrance_ids_to_add.append(new)
	
	var entrance_ids_to_remove = []
	for old in _entrances:
		if new_entrances.has(old):
			continue
		entrance_ids_to_remove.append(old)
	
	# Do pathfinding for new Entrances
	# Array of [Entrance, weights]
	var data_to_add = []
	for new_id in entrance_ids_to_add:
		# Get distances to each different Entrance in new_entrances
		var curr_entrance = new_entrances.get(new_id)
		var distances = _flood_fill_paths(curr_entrance.from)
		var data = [Entrance.new(to_global(curr_entrance.a), to_global(curr_entrance.b)), []]
		for other_id in new_entrances:
			if other_id == new_id:
				continue
			var other_entrance = new_entrances.get(other_id)
			var dist = distances.get(_get_point_id(other_entrance.from.x, other_entrance.from.y, other_entrance.from.z))
			if dist != null:
				data[1].append([other_id, dist])
		data_to_add.append(data)
	
	# Add to global pathfinding with weights based on the paths
	for data in data_to_add:
		_chunk_manager.add_entrance(data[0])
		_chunk_manager.set_weights(data[0].get_id(), data[1])
	
	_entrances = new_entrances

func _flood_fill_paths(startPos : Vector3i, stop_condition : Callable = func (a): false):
	var startId = _get_point_id(startPos.x, startPos.y, startPos.z)
	if !_pathfinding.has_point(startId):
		push_warning("Flood fill start point is not in graph!")
		return null
	
	var distances = {}
	
	var buffer = [startId]
	var visited = {}
	distances.set(startId, 0)
	while !buffer.is_empty():
		var curr = buffer.pop_front()
		var currDist = distances.get(curr)
		if stop_condition.call(curr):
			return distances
		for neighbor in _pathfinding.get_point_connections(curr):
			if !visited.get(neighbor, false) and !buffer.has(neighbor):
				buffer.push_back(neighbor)
				distances.set(neighbor, currDist + 1) # This will be just an estimation
		
		visited.set(curr, true)
	
	return distances

func total_transform(transformFunc : Callable):
	full_chunk_lock.write_lock()
	transformFunc.call(self)
	
	_calculate_pathfinding()
	full_chunk_lock.write_unlock()
	
	_chunk_manager.alert_neighbor_changed(self)

func get_global_path(from : Vector3i, to : Vector3i):
	var path = get_path(from_global(from), from_global(to))
	if path == null:
		return null
	
	return (path as Array).map(func (pos): return to_global(pos))

func get_path(from : Vector3i, to : Vector3i):
	var ida = _get_point_id(from.x, from.y, from.z)
	var idb = _get_point_id(to.x, to.y, to.z)
	
	if !_pathfinding.has_point(ida) || !_pathfinding.has_point(idb):
		return null
	
	return _pathfinding.get_point_path(ida, idb)

func add_manager(manager : ChunkManager):
	_chunk_manager = manager

func to_global(pos : Vector3i) -> Vector3i:
	var result = pos + position * dimensions
	return result

func from_global(pos : Vector3i) -> Vector3i:
	var result = pos - position * dimensions
	return result

func handle_neighbor_change():
	_create_entrances()

func get_closest_entrance(pos : Vector3i) -> Entrance:
	pos = from_global(pos)
	var closest = []
	_flood_fill_paths(pos, func (currPos):
		return _entrances.values().any(func (entrance): 
			if entrance.from == _get_point_by_id(currPos):
				closest.append(entrance)
				return true))
	
	if closest.is_empty():
		return null
	
	return Entrance.new(to_global(closest[0].a), to_global(closest[0].b))
