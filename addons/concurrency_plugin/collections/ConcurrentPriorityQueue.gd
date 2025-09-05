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
	return (await super.pop()).get(VALUE_KEY)

func push(value, prio : int = 50):
	_write_lock.lock()
	if _readers > 0:
		await _no_readers
	
	var idx = _data.bsearch_custom(_data, self._compare)
	_data.insert(idx, {VALUE_KEY: value, PRIORITY_KEY: prio})
	
	_new_data.emit()
	_write_lock.unlock()
