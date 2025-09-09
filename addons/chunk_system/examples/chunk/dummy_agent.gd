class_name DummyAgent extends Object

var pos : Vector3i
var chunk_manager : ChunkManager

func get_to(to : Vector3i):
	var abstract = chunk_manager.get_abstract_path(pos, to)
	print(abstract)
	while abstract.size() > 2:
		print(abstract)
		traverse(chunk_manager.get_partial_path(pos, abstract[2]))
		abstract.pop_front()
		abstract.pop_front()
	
	traverse(chunk_manager.get_path(pos, to))

func traverse(path : Array):
	for point in path:
		print("Going to: %s" % point)
		pos = point
	
