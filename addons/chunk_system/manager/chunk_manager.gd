class_name ChunkManager extends HierachicalPathfinder

var _threadPool : ThreadPool
var _chunks : Dictionary[Vector3i, Chunk] = {}
var _dimensions : Vector3i
var _pathfinding : AStar3D = AStar3D.new()
var _abstract_pf : HierarchicalAstar = HierarchicalAstar.new()

var pathfinding_lock : ReadWriteLock = ReadWriteLock.new().set_name("pathfinding")
var abstract_pf_lock : ReadWriteLock = ReadWriteLock.new().set_name("abstract")

func _init(threadPool : ThreadPool, dimensions : Vector3i) -> void:
	self._threadPool = threadPool
	self._dimensions = dimensions

func _get_chunk_by_global_pos(pos : Vector3i) -> Chunk:
	var chunk_pos : Vector3i = pos / _dimensions
	if pos.x < 0: chunk_pos.x -= 1
	if pos.y < 0: chunk_pos.y -= 1
	if pos.z < 0: chunk_pos.z -= 1
	
	return _chunks.get(chunk_pos)

func add_chunk(chunk : Chunk):
	if _chunks.has(chunk.position):
		push_warning("Chunk position collision")
		return
	
	_chunks.set(chunk.position, chunk)
	chunk.add_manager(self)

func total_transform(transformFunc : Callable):
	for chunk : Chunk in _chunks.values():
		_threadPool.execute(chunk.total_transform.bind(transformFunc), ChunkPrios.LARGE_UPDATE)

# Blocking
func get_global_val(pos : Vector3i) -> Holder:
	var chunk = _get_chunk_by_global_pos(pos)
	if chunk == null:
		return null
	
	var in_chunk_pos = pos % _dimensions
	
	chunk.full_chunk_lock.read_lock()
	var result = chunk.getValVect(in_chunk_pos)
	chunk.full_chunk_lock.read_unlock()
	
	return result

func try_get_global_val(pos : Vector3i) -> Holder:
	var chunk = _get_chunk_by_global_pos(pos)
	if chunk == null:
		return null
	
	var in_chunk_pos = pos % _dimensions
	
	if !chunk.full_chunk_lock.try_read_lock():
		return null
	
	var result = chunk.getValVect(in_chunk_pos)
	chunk.full_chunk_lock.read_unlock()
	
	return result

# Checks whether the given global position is an edge for
# any of the chunks
func try_global_has_edge(pos : Vector3i) -> bool:
	var chunk = _get_chunk_by_global_pos(pos)
	if chunk == null:
		return false
	
	var in_chunk_pos = pos % _dimensions
	
	chunk.full_chunk_lock.read_lock()
	
	var result = chunk.edges.has(in_chunk_pos)
	chunk.full_chunk_lock.read_unlock()
	
	return result

# Only alerting on same level for now
func alert_neighbor_changed(changedChunk : Chunk):
	for offset in [Vector3i(-1, 0, 0), Vector3i(1, 0, 0), Vector3i(0, 0, -1), Vector3i(0, 0, 1)]:
		var chunk : Chunk = _chunks.get(changedChunk.position + offset)
		if chunk == null:
			continue
		
		_threadPool.execute(chunk.handle_neighbor_change, ChunkPrios.LARGE_UPDATE)

func add_entrance(entrance : Entrance):
	abstract_pf_lock.write_lock()
	var id = entrance.get_id()
	if _abstract_pf.has_point(id):
		abstract_pf_lock.write_unlock()
		return
	
	print("Adding entrance - from: %s to: %s" % [entrance.a, entrance.b])
	
	_abstract_pf.add_point(id, entrance.a)
	var other_id = entrance.get_reverse_id()
	if !_abstract_pf.has_point(other_id):
		_abstract_pf.add_point(other_id, entrance.b)
		_abstract_pf.connect_points(id, other_id)
		_abstract_pf.set_weight(id, other_id, 1)
		_abstract_pf.set_weight(other_id, id, 1)
	abstract_pf_lock.write_unlock()
	
	pathfinding_lock.write_lock()
	var ida = ChunkUtil.get_point_id(entrance.a, _dimensions)
	var idb = ChunkUtil.get_point_id(entrance.b, _dimensions)
	_pathfinding.add_point(ida, entrance.a)
	_pathfinding.add_point(idb, entrance.b)
	_pathfinding.connect_points(ida, idb)
	pathfinding_lock.write_unlock()
	

# weights : Array of [to_id, weight]
func set_weights(from_id : int, weights : Array):
	print("SETTING WEIGHTS")
	print(weights)
	abstract_pf_lock.write_lock()
	for data : Array in weights:
		_abstract_pf.set_weight(from_id, data[0], data[1])
		if !_abstract_pf.has_point(data[0]):
			continue
		print("Connecting - from: %s to: %s" % [_abstract_pf.get_point_position(from_id), _abstract_pf.get_point_position(data[0])])
		_abstract_pf.connect_points(from_id, data[0])
	abstract_pf_lock.write_unlock()

func set_points(chunk : Chunk, points : Array):
	pathfinding_lock.write_lock()
	ChunkUtil.remove_chunk_from_pathfinding(chunk, _pathfinding)
	
	for point : Vector3i in points:
		var id = ChunkUtil.get_point_id(point, _dimensions)
		_pathfinding.add_point(id, point)
	
	join_points(chunk, points)
	pathfinding_lock.write_unlock()

func join_points(chunk : Chunk, points : Array):
	for pos in points:
		var id = ChunkUtil.get_point_id(pos, chunk.dimensions)
		for offset in [[1, 0, 0], [0, 1, 0], [0, 0, 1], [1, 1, 0], [0, 1, 1], [1, -1, 0], [0, -1, 1]]:
			if pos.z + offset[2] == _dimensions.z:
				continue
			var otherId = ChunkUtil.get_point_id(Vector3i(pos.x + offset[0], pos.y + offset[1], pos.z + offset[2]), chunk.dimensions)
			if _pathfinding.has_point(otherId):
				var a = ChunkUtil.get_point_by_id(id, _dimensions)
				var b = ChunkUtil.get_point_by_id(otherId, _dimensions)
				if (a - b).length() > 1.5:
					push_error("Connecting %s to %s with length greater than 1.5%" % [a, b])
				_pathfinding.connect_points(id, otherId, true)

func get_abstract_path(from : Vector3i, to : Vector3i) -> Array:
	print("Getting abstract path from %s to %s" % [from, to])
	# Check if from and to are in the same chunk
	var from_chunk = _get_chunk_by_global_pos(from)
	var to_chunk = _get_chunk_by_global_pos(to)
	var from_id = ChunkUtil.get_point_id(from, _dimensions)
	var to_id = ChunkUtil.get_point_id(to, _dimensions)
	if from_chunk == null or to_chunk == null:
		return []
	
	if from_chunk == to_chunk or (from_chunk.position - to_chunk.position).length() == 1:
		return []
	
	var abstract_start = _abstract_pf.get_closest_point(from)
	var abstract_end = _abstract_pf.get_closest_point(to)
	print("Found abstract start and goal: from %s to %s" % [_abstract_pf.get_point_position(abstract_start), _abstract_pf.get_point_position(abstract_end)])
	print("Start connections: %s" % _abstract_pf.get_point_connections(abstract_start))
	print("Goal connections: %s" % _abstract_pf.get_point_connections(abstract_end))
	
	return _abstract_pf.get_point_path(abstract_start, abstract_end)

func get_path(from : Vector3i, to : Vector3i):
	print("Trying to get path from %s to %s" % [from, to])
	var from_id = ChunkUtil.get_point_id(from, _dimensions)
	var to_id = ChunkUtil.get_point_id(to, _dimensions)
	if !_pathfinding.has_point(from_id) or !_pathfinding.has_point(to_id):
		return []
	
	return _pathfinding.get_point_path(from_id, to_id, true)

# Get path in current chunk + 1
func get_partial_path(from : Vector3i, to : Vector3i):
	print("Getting partial path")
	var from_chunk = _get_chunk_by_global_pos(from)
	var path = get_path(from, to)
	var partial = []
	for pos in path:
		partial.append(pos)
		if !from_chunk._is_in_global_bounds(pos):
			break
	
	return partial
