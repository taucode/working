(defblock :name dispose-worker :is-top t
	(executor
		:executor-name dispose-worker
		:verb "dispose"
		:description "Disposes worker with the given name."
		:usage-samples (
			"dispose my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
