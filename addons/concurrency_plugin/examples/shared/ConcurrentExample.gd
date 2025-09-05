class_name ConcurrentExample extends Node

const STOP_SIGNAL = -1
const NUMBER = 100

var queue : ConcurrentPriorityQueue = ConcurrentPriorityQueue.new()

func push_stop():
	queue.push(STOP_SIGNAL, -1)

func create_producer():
	var producer = Thread.new()
	producer.start(self.producer_func)
	
	return producer

func producer_func():
	for i in range(NUMBER):
		queue.push(1)
		
		OS.delay_msec(7)
	
	print("Producer finished")

func create_prio_producer():
	var producer = Thread.new()
	producer.start(self.prio_producer_func)
	
	return producer

func prio_producer_func():
	for i in range(NUMBER):
		queue.push(i, i)
		
		OS.delay_msec(7)
	
	print("Producer finished")

func create_consumer(print : bool = false):
	var consumer = Thread.new()
	consumer.start(self.consumer_func.bind(print))
	
	return consumer

func consumer_func(print : bool = false):
	var count = 0
	var lastData = 0
	
	while lastData != STOP_SIGNAL:
		lastData = queue.pop()
		count += lastData
		
		if print:
			print(lastData)
		
		OS.delay_msec(5)
	
	print("Consumer finished: %d" % count)
