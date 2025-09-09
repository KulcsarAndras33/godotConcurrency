class_name Entrance extends Object

var a : Vector3i
var b : Vector3i
var from : Vector3i

func _init(a : Vector3i, b : Vector3i):
	self.from = a
	self.a = a
	self.b = b
	
	if a == b:
		push_error("The two positions of an entrance are the same: %s" % a)
	#elif ChunkUtil.compare_vectors(a, b):
		#self.a = a
		#self.b = b
	#else:
		#self.a = b
		#self.b = a

func get_id() -> int:
	var vals = [a.x, a.y, a.z, b.x, b.y, b.z]
	var hash = vals[0]
	for val in vals:
		var x = val
		x = ((x >> 32) ^ x) * 0x45d9f3b
		x = ((x >> 32) ^ x) * 0x45d9f3b
		x = (x >> 32) ^ x
		hash ^= x + 0x9e3779b9 + (hash << 6) + (hash >> 2)
	
	return abs(hash)

func get_reverse_id() -> int:
	var vals = [b.x, b.y, b.z, a.x, a.y, a.z]
	var hash = vals[0]
	for val in vals:
		var x = val
		x = ((x >> 32) ^ x) * 0x45d9f3b
		x = ((x >> 32) ^ x) * 0x45d9f3b
		x = (x >> 32) ^ x
		hash ^= x + 0x9e3779b9 + (hash << 6) + (hash >> 2)
	
	return abs(hash)

func equals(other : Entrance) -> bool:
	if a == other.a and b == other.b:
		return true
	
	if a == other.b and b == other.a:
		push_warning("Two Entrances have the same positions in reverse.")
		return true
	
	return false
