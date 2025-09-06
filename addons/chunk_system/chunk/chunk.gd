class_name Chunk extends Object

var _data : Array
var _pathfinding : AStar3D = AStar3D.new()

var dimensions : Vector3i
var position : Vector3i
var full_chunk_lock : ReadWriteLock = ReadWriteLock.new()

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
	return x * dimensions.x + y * dimensions.y + z

# Only call from locked
func _add_point_to_pathfinding(x : int, y : int, z : int):
	var id = _get_point_id(x, y, z)
	_pathfinding.add_point(id, Vector3(x, y, z))
	
	for offset in [[1, 0, 0], [0, 1, 0], [0, 0, 1]]:
		var otherId = _get_point_id(x + offset[0], y + offset[0], z + offset[0])
		if _pathfinding.has_point(otherId):
			_pathfinding.connect_points(id, otherId)

func _calculate_pathfinding():
	full_chunk_lock.writeLock()
	_pathfinding.clear()
	
	for x in range(dimensions.x):
		for y in range(dimensions.y):
			for z in range(dimensions.z):
				var val = _getVal(x, y, z)
				if val == null:
					continue
				
				if val.pass_through:
					_add_point_to_pathfinding(x, y, z)
					continue
				
				var under = _getVal(x, y - 1, z)
				if under != null && under.is_walkable:
					_add_point_to_pathfinding(x, y, z)
	
	full_chunk_lock.writeUnlock()

func total_transform(transformFunc : Callable):
	full_chunk_lock.writeLock()
	transformFunc.call()
	
	_calculate_pathfinding()
	full_chunk_lock.writeUnlock()
