class_name Chunk extends Object

var _data : Array

var position : Vector3i
var full_chunk_lock : Mutex = Mutex.new()

func _init(position : Vector3i, dimensions : Vector3i):
	self.position = position
	
	_init_data(dimensions)

func _init_data(dimensions : Vector3i):
	for x in range(dimensions.x):
		_data.append([])
		for y in range(dimensions.y):
			_data[x].append([])
