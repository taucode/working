(defblock :name get-name-of-worker :is-top t
	(executor
		:executor-name get-name-of-worker
		:verb "get-name"
		:description "Gets name of the worker with the given name. Looks weird, and it is."
		:usage-samples (
			"get-name my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
