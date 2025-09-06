extends Node

func _ready() -> void:
	var threadPool = ThreadPool.new(4)
	var chunk_manager = ChunkManager.new(threadPool, Vector3i(5, 2, 5))
	var chunk = Chunk.new(Vector3i(0,0,0), Vector3i(5, 2, 5))
	var otherChunk = Chunk.new(Vector3i(1,0,0), Vector3i(5, 2, 5))
	
	chunk_manager.add_chunk(chunk)
	chunk_manager.add_chunk(otherChunk)
	
	chunk_manager.total_transform(self.chunk_random_fill.bind([0], [0.75]))
	
	await get_tree().create_timer(1).timeout
	
	print_level(0, chunk)
	print("===============")
	print_level(0, otherChunk)
	
	print(chunk.get_path_between(Vector3i(0,1,0), Vector3i(4,1,4)))
	

func chunk_random_fill(chunk : Chunk, levels, probs):
	for i in range(levels.size()):
		var y = levels[i]
		for x in range(chunk.dimensions.x):
			for z in range(chunk.dimensions.z):
				if randf() < probs[i]:
					chunk._data[x][y][z] = Holder.new()

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
