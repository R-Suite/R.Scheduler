DROP TABLE IF EXISTS rsched_custom_jobs;


CREATE TABLE rsched_custom_jobs
(
  id uuid NOT NULL,
  name character varying(200) NOT NULL,
  params text,
  job_type character varying(200) NOT NULL,
  CONSTRAINT rsched_custom_jobs_pkey PRIMARY KEY (id),
  CONSTRAINT rsched_custom_jobs_name_job_type_key UNIQUE (name, job_type)
)