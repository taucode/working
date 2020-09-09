(defblock :name pause-worker :is-top t
	(executor
		:executor-name pause-worker
		:verb "pause"
		:description "Pauses worker with the given name."
		:usage-samples (
			"pause my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
