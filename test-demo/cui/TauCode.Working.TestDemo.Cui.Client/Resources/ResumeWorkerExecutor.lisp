(defblock :name resume-worker :is-top t
	(executor
		:executor-name resume-worker
		:verb "resume"
		:description "Resumes worker with the given name."
		:usage-samples (
			"resume my-good-worker"))

	(some-text
		:classes term string
		:action argument
		:alias worker-name
		:description "Worker name."
		:doc-subst "worker name")

	(end)
)
