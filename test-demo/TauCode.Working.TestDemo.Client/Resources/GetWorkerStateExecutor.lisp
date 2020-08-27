(defblock :name get-state-of-worker :is-top t
	(executor
		:executor-name get-state-of-worker
		:verb "get-state"
		:description "Gets state of the worker with the given name."
		:usage-samples (
			"get-state my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
