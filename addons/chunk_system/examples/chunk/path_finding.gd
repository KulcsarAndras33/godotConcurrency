extends Node

func _ready() -> void:
	path()

func time():
	const N = 5000
	const M = 100000
	var dict = {}
	var keyArr = []
	var valArr = []
	var keys = []
	for i in range(N):
		var rand = [i, N + i]
		keys.append(rand)
		dict.set(rand, 5)
		var idx = keyArr.bsearch(rand)
		keyArr.insert(idx, rand)
		valArr.insert(idx, 5)
	
	var time = 0
	for i in range(M):
		var s = Time.get_ticks_usec()
		dict.get(keys[M % N])
		var e = Time.get_ticks_usec()
		time += e - s
	print("Dictionary: %s" % time)


func path():
	var threadPool = ThreadPool.new(4)
	var chunk_dimensions = Vector3i(20, 2, 20)
	var chunk_manager = ChunkManager.new(threadPool, chunk_dimensions)
	
	var chunks = []
	for i in range(100):
		var chunk = Chunk.new(Vector3i(i, 0, 0), chunk_dimensions)
		chunk_manager.add_chunk(chunk)
		chunks.append(chunk)
	
	chunk_manager.total_transform(self.chunk_random_fill.bind([0], [0.9]))
	
	await get_tree().create_timer(5).timeout
	await threadPool.shutdown()
	
	#var visualizer : NaiveChunkVisualizer = $Visualizer
	#visualizer.set_chunks(chunks)
	#visualizer.create()
	
	#print_level(0, chunk)
	#print("===============")
	#print_level(0, otherChunk)
	#print("===============")
	#print_level(0, chunk3)
	
	print("Normal pathfinding point count: %d" % chunk_manager._pathfinding.get_point_count())
	
	var goal = Vector3i(99 * 20 + 1, 1, 10)
	var start = Time.get_ticks_usec()
	chunk_manager.get_path(Vector3i(0,1,0), goal)
	var end = Time.get_ticks_usec()
	print("Normal pathfinding: %d micros" % (end - start))
	
	var agent = $Agent
	agent.pos = Vector3i(0,1,0)
	agent.chunk_manager = chunk_manager
	
	agent.get_to(goal)
	
	print("Total dict get time: %s" % chunk_manager._abstract_pf.time)

func chunk_random_fill(chunk : Chunk, levels, probs):
	for i in range(levels.size()):
		var y = levels[i]
		for x in range(chunk.dimensions.x):
			for z in range(chunk.dimensions.z):
				if randf() < probs[i]:
					chunk._data[x][y][z] = Holder.new()
		chunk._data[0][i][0] = Holder.new()

func print_level(y : int, chunk : Chunk):
	for x in range(chunk.dimensions.x):
		var buffer = []
		for z in range(chunk.dimensions.z):
			var data = chunk._data[x][y][z]
			if data == null:
				buffer.append(0)
				continue
			
			buffer.append(chunk._data[x][y][z].is_walkable)
		print(buffer)
