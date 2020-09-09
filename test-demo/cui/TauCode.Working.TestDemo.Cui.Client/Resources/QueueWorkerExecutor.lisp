(defblock :name worker-queue :is-top t
	(executor
		:executor-name worker-queue
		:verb "queue"
		:description "Manage queue worker."
		:usage-samples (
			"queue my-worker 100 200"
			"queue my-worker -1 -1 1000"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(some-integer
		:action argument
		:alias queue-from
		:description "Queue from."
		:doc-subst "queue from")

	(some-integer
		:action argument
		:alias queue-to
		:description "Queue to."
		:doc-subst "queue to")

	(opt
		(some-integer
			:action argument
			:alias job-delay
			:description "Job delay."
			:doc-subst "job delay")
	)

	(end)
)
