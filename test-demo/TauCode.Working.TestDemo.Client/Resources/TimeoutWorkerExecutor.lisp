(defblock :name worker-timeout :is-top t
	(executor
		:executor-name worker-timeout
		:verb "timeout"
		:description "Get and/or set timeout of a timeout-based worker."
		:usage-samples (
			"timeout my-worker"
			"timeout my-worker 1000"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(opt
		(some-integer
			:classes term
			:action argument
			:alias timeout-value
			:description "Timeout value."
			:doc-subst "timeout value")
	)

	(end)
)
