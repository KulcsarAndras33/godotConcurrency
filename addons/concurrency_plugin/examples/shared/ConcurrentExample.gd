class_name ConcurrentExample extends Node

const STOP_SIGNAL = -1
const NUMBER = 100

var queue : ConcurrentPriorityQueue = ConcurrentPriorityQueue.new()

func push_stop():
	queue.push(STOP_SIGNAL)

func create_producer():
	var producer = Thread.new()
	producer.start(self.producer_func)
	
	return producer

func producer_func():
	for i in range(NUMBER):
		queue.push(1)
		
		await get_tree().create_timer(0.0075).timeout
	
	print("Producer finished")

func create_prio_producer():
	var producer = Thread.new()
	producer.start(self.prio_producer_func)
	
	return producer

func prio_producer_func():
	for i in range(NUMBER):
		queue.push(i, i)
		
		await get_tree().create_timer(0.0075).timeout
	
	print("Producer finished")

func create_consumer(print : bool = false):
	var consumer = Thread.new()
	consumer.start(self.consumer_func.bind(print))
	
	return consumer

func consumer_func(print : bool = false):
	var count = 0
	var lastData = 0
	
	while lastData != STOP_SIGNAL:
		lastData = await queue.pop()
		count += lastData
		
		if print:
			print(lastData)
		
		await get_tree().create_timer(0.005).timeout
	
	print("Consumer finished: %d" % count)
