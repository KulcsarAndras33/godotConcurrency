class_name Holder extends Object

const DATA_KEY = "data"
const WALKABLE_KEY = "is_walkable"
const PASS_THORUGH_KEY = "pass_through"

var data
var is_walkable : bool = true
var pass_through : bool = false

func configure(config : Dictionary) -> Holder:
	if config.has(DATA_KEY):
		data = config.get(DATA_KEY)
	if config.has(WALKABLE_KEY):
		is_walkable = config.get(WALKABLE_KEY)
	if config.has(PASS_THORUGH_KEY):
		pass_through = config.get(PASS_THORUGH_KEY)
	
	return self
