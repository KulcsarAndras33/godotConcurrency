class_name ChunkUtil extends Object

# Is A smaller then B?
static func compare_vectors(a : Vector3i, b : Vector3i) -> bool:
	if a == b:
		return false
	
	var diff_sign = (b - a).sign()
	var diffs = [diff_sign.x, diff_sign.y, diff_sign.z]
	var sign_sum = diffs.reduce(func (accum, n): return accum + n, 0)
	
	if sign_sum > 0:
		return true
	elif sign_sum < 0:
		return false
	else:
		for diff in diffs:
			if diff != 0:
				return diff == 1
	
	return false

static func get_point_id(point: Vector3i, dim : Vector3i) -> int:
	return point.x * (dim.y * dim.z) + point.y * dim.z + point.z

static func get_point_by_id(id : int, dim : Vector3i) -> Vector3i:
	var x : int = id / (dim.y * dim.z)
	var rem = id % (dim.y * dim.z)
	var y : int = rem / dim.z
	var z = rem % dim.z
	return Vector3i(x, y, z)

# TAKE CARE OF LOCKING!
static func remove_chunk_from_pathfinding(chunk : Chunk, pf : AStar3D):
	var closest = pf.get_closest_point(chunk.to_global(chunk.dimensions / 2))
	if !chunk._is_global_in_bounds_by_id(closest):
		return
	
	var distances = ChunkUtil.flood_fill(chunk, pf, closest)
	for p in distances.keys():
		pf.remove_point(p)

static func flood_fill(chunk : Chunk, pf : AStar3D, start : int) -> Dictionary:
	if !pf.has_point(start):
		push_error("Starting flood fill with nonexistent point id.")
		return {}
	
	var buffer = [start]
	var visited = {}
	var distances = {start: 0}
	while !buffer.is_empty():
		var curr = buffer.pop_back()
		var dist = distances.get(curr)
		for neighbor in pf.get_point_connections(curr):
			if chunk._is_in_bounds_by_id(curr) and !visited.has(neighbor) and !buffer.has(neighbor):
				buffer.push_front(neighbor)
				distances.set(neighbor, dist + 1)
		visited.set(curr, true)
	
	return distances
