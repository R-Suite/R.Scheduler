DROP TABLE IF EXISTS rsched_audit_history;

CREATE TABLE rsched_audit_history
(
  time_stamp date,
  action character varying(200) NOT NULL,
  fire_instance_id character varying(200) NOT NULL,
  job_name character varying(200),
  job_group character varying(200),
  job_type character varying(200),
  trigger_name character varying(150),
  trigger_group character varying(150),
  fire_time_utc timestamp with time zone,
  scheduled_fire_time_utc timestamp with time zone,
  job_run_time bigint,
  params text,
  refire_count integer,
  recovering boolean,
  result text,
  execution_exception text,
  CONSTRAINT rsched_audit_history_pkey PRIMARY KEY (action, fire_instance_id)
)


DROP TABLE IF EXISTS rsched_job_id_key_map;

CREATE TABLE rsched_job_id_key_map
(
  id uuid NOT NULL,
  job_name character varying(200),
  job_group character varying(200),
  CONSTRAINT rsched_job_id_key_map_pkey PRIMARY KEY (id)
)


DROP TABLE IF EXISTS rsched_trigger_id_key_map;

CREATE TABLE rsched_trigger_id_key_map
(
  id uuid NOT NULL,
  trigger_name character varying(200),
  trigger_group character varying(200),
  CONSTRAINT rsched_trigger_id_key_map_pkey PRIMARY KEY (id)
)


DROP TABLE IF EXISTS rsched_calendar_id_name_map

CREATE TABLE rsched_calendar_id_name_map
(
  id uuid NOT NULL,
  calendar_name character varying(200),
  CONSTRAINT rsched_calendar_id_name_map_pkey PRIMARY KEY (id)
)