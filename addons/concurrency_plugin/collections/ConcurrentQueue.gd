class_name ConcurrentQueue extends Object

signal _no_readers
signal _new_data

var _data : Array = []
var _write_lock : Mutex = Mutex.new()		# 1.
var _read_lock : Mutex = Mutex.new()		# 2.
var _readers : int = 0

func _addReader():
	_write_lock.lock()
	_read_lock.lock()
	_readers += 1
	_read_lock.unlock()
	_write_lock.unlock()

func _removeReader():
	_read_lock.lock()
	_readers -= 1
	_read_lock.unlock()
	
	if _readers == 0:
		_no_readers.emit()

func peek():
	self._addReader()
	var value = _data.back()
	self._removeReader()
	
	return value

func pop():
	while _data.is_empty() || !_write_lock.try_lock():
		await _new_data
	
	if _readers > 0:
		await _no_readers
	
	var value = _data.pop_back()
	_write_lock.unlock()
	
	return value

func push(value):
	_write_lock.lock()
	if _readers > 0:
		await _no_readers
	
	_data.push_front(value)
	_new_data.emit()
	_write_lock.unlock()
