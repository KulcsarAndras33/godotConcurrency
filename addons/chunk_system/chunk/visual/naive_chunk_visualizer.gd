class_name NaiveChunkVisualizer extends Node

var BLOCK_SCENE = preload("res://addons/chunk_system/chunk/visual/block.tscn")

var _chunks : Array[Chunk]

func _createChunk(chunk: Chunk):
	chunk.full_chunk_lock.read_lock()
	for x in range(chunk.dimensions.x):
		for y in range(chunk.dimensions.y):
			for z in range(chunk.dimensions.z):
				var data = chunk._getVal(x, y, z)
				if data == null:
					continue
				
				var block : Node3D = BLOCK_SCENE.instantiate()
				add_child(block)
				block.global_position = chunk.position * chunk.dimensions + Vector3i(x, y, z)
				
	chunk.full_chunk_lock.read_unlock()

func set_chunks(chunks : Array[Chunk]) -> void:
	_chunks = chunks

func create():
	for chunk in _chunks:
		_createChunk(chunk)
