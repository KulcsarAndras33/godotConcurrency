class_name AtomicInteger extends Object

var _lock : Mutex = Mutex.new()
var _value : int

func _init(value : int = 0):
	_value = value

func val() -> int:
	return _value

func setVal(value : int):
	_lock.lock()
	_value = value
	_lock.unlock()

func change(delta : int):
	_lock.lock()
	_value += delta
	_lock.unlock()
