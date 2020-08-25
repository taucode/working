(defblock :name start-worker :is-top t
	(executor
		:executor-name start-worker
		:verb "start"
		:description "Starts worker with the given name."
		:usage-samples (
			"wrk start my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
