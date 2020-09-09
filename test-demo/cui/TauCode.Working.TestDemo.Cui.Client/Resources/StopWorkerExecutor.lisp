(defblock :name stop-worker :is-top t
	(executor
		:executor-name stop-worker
		:verb "stop"
		:description "Stops worker with the given name."
		:usage-samples (
			"stop my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
