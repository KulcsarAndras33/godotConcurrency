class_name ChunkManager extends HierachicalPathfinder

var _threadPool : ThreadPool
var _chunks : Dictionary[Vector3i, Chunk] = {}
var _dimensions : Vector3i
var _pathfinding : HierarchicalAstar = HierarchicalAstar.new()

func _init(threadPool : ThreadPool, dimensions : Vector3i) -> void:
	self._threadPool = threadPool
	self._dimensions = dimensions

func _get_chunk_by_global_pos(pos : Vector3i) -> Chunk:
	var chunk_pos = pos / _dimensions
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
	
	if !chunk.full_chunk_lock.try_read_lock():
		return false
	
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

# TODO SYNC
func add_entrance(entrance : Entrance):
	var id = entrance.get_id()
	if _pathfinding.has_point(id):
		return
	
	print("Adding entrance - from: %s to: %s" % [entrance.a, entrance.b])
	
	_pathfinding.add_point(id, entrance.a)

# TODO SYNC
# weights : Array of [to_id, weight]
func set_weights(from_id : int, weights : Array):
	for data : Array in weights:
		if !_pathfinding.has_point(data[0]):
			continue
		_pathfinding.connect_points(from_id, data[0])
		_pathfinding.set_weight(from_id, data[0], data[1])

func get_path(from : Vector3i, to : Vector3i):
	# Check if from and to are in the same chunk
	var from_chunk = _get_chunk_by_global_pos(from)
	var to_chunk = _get_chunk_by_global_pos(to)
	if from_chunk == null or to_chunk == null:
		return []
	
	if from_chunk == to_chunk:
		return from_chunk.get_global_path(from, to)
	
	var closest_start_entrance = from_chunk.get_closest_entrance(from)
	var closest_goal_entrance = to_chunk.get_closest_entrance(to)
	
	print("%s - %s" % [closest_start_entrance.a, closest_start_entrance.b])
	print("%s - %s" % [closest_goal_entrance.a, closest_goal_entrance.b])
	
	print(_pathfinding.has_point(closest_start_entrance.get_id()))
	print(_pathfinding.has_point(closest_goal_entrance.get_id()))
	
	return _pathfinding.get_point_path(closest_start_entrance.get_id(), closest_goal_entrance.get_id())
	
