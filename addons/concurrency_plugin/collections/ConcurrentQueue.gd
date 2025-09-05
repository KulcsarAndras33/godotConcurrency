class_name ConcurrentQueue extends Object

var _no_readers = Semaphore.new()
var _new_data = Semaphore.new()

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
		if !_no_readers.try_wait():
			_no_readers.post()

func peek():
	self._addReader()
	if _data.is_empty():
		return null
	
	var value = _data.back()
	self._removeReader()
	
	return value

func pop():
	while true:
		if _data.is_empty():
			_new_data.wait()
		else:
			if _write_lock.try_lock():
				break
	
	if _readers > 0:
		_no_readers.wait()
	
	var value = _data.pop_back()
	_write_lock.unlock()
	
	return value

func push(value, prio : int = -1):
	if prio != -1:
		push_warning("Might want to use ConcurrentPriorityQueue instead")
	
	_write_lock.lock()
	if _readers > 0:
		_no_readers.wait()
	
	_data.push_front(value)
	if !_new_data.try_wait():
		_new_data.post()
	_write_lock.unlock()
