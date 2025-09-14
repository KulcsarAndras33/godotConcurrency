class_name DummyAgent extends Node3D

var pos : Vector3i
var chunk_manager : ChunkManager
var time = 0

func get_to(to : Vector3i):
	var start = Time.get_ticks_usec()
	var abstract = chunk_manager.get_abstract_path(pos, to)
	var end = Time.get_ticks_usec()
	time += end - start
	print("Calculating abstract path took: %s microsec" % time)
	#print(abstract)
	while abstract.size() > 2:
		print(abstract)
		start = Time.get_ticks_usec()
		var partial = chunk_manager.get_partial_path(pos, abstract[2])
		end = Time.get_ticks_usec()
		time += end - start
		traverse(partial)
		abstract.pop_front()
		abstract.pop_front()
	
	start = Time.get_ticks_usec()
	var final = chunk_manager.get_path(pos, to)
	end = Time.get_ticks_usec()
	time += end - start
	print("Total time for agent: %d micros" % time)
	
	traverse(final)

func traverse(path : Array):
	for point in path:
		#print("Going to: %s" % point)
		pos = point
		global_position = pos
		#await get_tree().create_timer(0.01).timeout
	
