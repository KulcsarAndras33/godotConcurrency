class_name ConcurrentPriorityQueue extends ConcurrentQueue

const PRIORITY_KEY = "prio"
const VALUE_KEY = "value"

func _compare(a : Dictionary, b : Dictionary):
	return a.get(PRIORITY_KEY) < b.get(PRIORITY_KEY)

func peek():
	var value = super.peek()
	if !value:
		return value
	
	return value.get(VALUE_KEY)

func pop():
	return super.pop().get(VALUE_KEY)

func push(value, prio : int = 50):
	_write_lock.lock()
	if _readers > 0:
		_no_readers.wait()
	
	var dict = {VALUE_KEY: value, PRIORITY_KEY: prio}
	
	var idx = _data.bsearch_custom(dict, self._compare)
	_data.insert(idx, dict)
	
	if !_new_data.try_wait():
		_new_data.post()
	_write_lock.unlock()
