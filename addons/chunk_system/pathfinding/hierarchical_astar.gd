class_name HierarchicalAstar extends AStar3D

# The weights are bidirectional
var _weights = {}
var connections_checked = 0
var time = 0

func _compute_cost(from_id: int, to_id: int) -> float:
	return _weights.get([min(from_id, to_id), max(from_id, to_id)])
	
	connections_checked += 1
	var id1 = min(from_id, to_id)
	var id2 = max(from_id, to_id)
	var key = [id1, id2]
	var weight = _weights.get(key)
	var s = Time.get_ticks_usec()
	_weights.get(key)
	var e = Time.get_ticks_usec()
	time += e - s
	if weight == null:
		push_error("There was no weight for the given edge.")
	#
	#var point_weights = _weights.get(id1)
	#if point_weights == null:
		#push_error("There was no weight for the given edge.")
	#
	#var weight = point_weights.get(id2)
	#if weight == null:
		#push_error("There was no weight for the given edge.")
	
	return weight

func set_weight(from_id : int, to_id: int, weight : int):
	var id1 = min(from_id, to_id)
	var id2 = max(from_id, to_id)
	
	_weights.set([id1, id2], weight)
	
	#var point_weights = _weights.get_or_add(id1, {})
	#point_weights.set(id2, weight)
