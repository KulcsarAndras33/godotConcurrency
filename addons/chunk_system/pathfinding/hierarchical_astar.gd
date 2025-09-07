class_name HierarchicalAstar extends AStar3D

# The weights are bidirectional
var _weights = {}

func _compute_cost(from_id: int, to_id: int) -> float:
	var id1 = min(from_id, to_id)
	var id2 = max(from_id, to_id)
	
	var point_weights = _weights.get(id1)
	if point_weights == null:
		push_error("There was no weight for the given edge.")
	
	var weight = point_weights.get(id2)
	if weight == null:
		push_error("There was no weight for the given edge.")
	
	return weight

func set_weight(from_id : int, to_id: int, weight : int):
	var id1 = min(from_id, to_id)
	var id2 = max(from_id, to_id)
	
	var point_weights = _weights.get_or_add(id1, {})
	point_weights.set(id2, weight)
