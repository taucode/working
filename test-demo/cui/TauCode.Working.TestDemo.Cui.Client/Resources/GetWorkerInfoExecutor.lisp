(defblock :name get-worker-info :is-top t
	(executor
		:executor-name get-worker-info
		:verb "get-info"
		:description "Gets worker info by its name."
		:usage-samples (
			"get-info my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
