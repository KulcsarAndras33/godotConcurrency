class_name ChunkUtil extends Object

# Is A smaller then B?
static func compare_vectors(a : Vector3i, b : Vector3i) -> bool:
	if a == b:
		return false
	
	var diff_sign = (b - a).sign()
	var diffs = [diff_sign.x, diff_sign.y, diff_sign.z]
	var sign_sum = diffs.reduce(func (accum, n): return accum + n, 0)
	
	if sign_sum > 0:
		return true
	elif sign_sum < 0:
		return false
	else:
		for diff in diffs:
			if diff != 0:
				return diff == 1
	
	return false
