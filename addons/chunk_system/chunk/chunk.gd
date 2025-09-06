class_name Chunk extends Object

var _data : Array
var _pathfinding : AStar3D = AStar3D.new()

var dimensions : Vector3i
var position : Vector3i
var full_chunk_lock : ReadWriteLock = ReadWriteLock.new()
var path_finding_filter : Callable = self._default_path_finding_filter

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
	full_chunk_lock.writeLock()
	_pathfinding.clear()
	
	for x in range(dimensions.x):
		for y in range(dimensions.y):
			for z in range(dimensions.z):
				if path_finding_filter.call(x, y, z):
					_add_point_to_pathfinding(x, y, z)
	
	_join_pathfinding_points()
	
	full_chunk_lock.writeUnlock()

func total_transform(transformFunc : Callable):
	full_chunk_lock.writeLock()
	transformFunc.call(self)
	
	_calculate_pathfinding()
	full_chunk_lock.writeUnlock()

func get_path_between(from : Vector3i, to : Vector3i):
	var ida = _get_point_id(from.x, from.y, from.z)
	var idb = _get_point_id(to.x, to.y, to.z)
	
	if !_pathfinding.has_point(ida) || !_pathfinding.has_point(idb):
		return null
	
	return _pathfinding.get_point_path(ida, idb)
