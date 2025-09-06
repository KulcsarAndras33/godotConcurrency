extends Node

func _ready() -> void:
	var chunk = Chunk.new(Vector3i(0,0,0), Vector3i(5, 3, 5))
	
	chunk.total_transform(self.chunk_random_fill.bind([0, 1], [0.75, 0.3]))
	
	print_level(0, chunk)
	print("============")
	print_level(1, chunk)
	
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
