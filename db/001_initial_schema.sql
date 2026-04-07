create extension if not exists "pgcrypto";

create table if not exists staff_users (
  user_id uuid primary key default gen_random_uuid(),
  username text unique not null,
  password_hash text not null,
  role text not null,
  department_id text,
  full_name text not null,
  is_active boolean default true,
  last_login_at timestamptz,
  created_at_utc timestamptz default now()
);

create table if not exists patients (
  patient_id uuid primary key default gen_random_uuid(),
  first_name text not null,
  last_name text not null,
  date_of_birth timestamptz not null,
  mrn text unique not null,
  contact_number text,
  email text,
  gender text not null,
  allergies_list jsonb default '[]'::jsonb,
  active_problems jsonb default '[]'::jsonb,
  is_active boolean default true,
  is_temporary boolean default false,
  is_john_doe boolean default false,
  created_at_utc timestamptz default now(),
  updated_at_utc timestamptz default now()
);

create table if not exists queue_models (
  model_id uuid primary key default gen_random_uuid(),
  model_type text unique not null,
  arrival_distribution text not null,
  service_distribution text not null,
  default_sigma_s2 double precision,
  default_sigma_a2 double precision,
  is_active boolean default true
);

create table if not exists encounters (
  encounter_id uuid primary key default gen_random_uuid(),
  patient_id uuid not null references patients(patient_id),
  queue_model_id uuid references queue_models(model_id),
  encounter_type text not null,
  arrival_time timestamptz not null,
  triage_category text,
  estimated_wait_min numeric,
  actual_wait_min numeric,
  current_state text not null,
  final_disposition text,
  discharge_time timestamptz,
  assigned_room text,
  assigned_doctor_id text,
  blood_pressure_systolic real,
  blood_pressure_diastolic real,
  heart_rate real,
  temperature real,
  sp_o2 real,
  created_at_utc timestamptz default now(),
  updated_at_utc timestamptz default now()
);

create table if not exists queue_snapshots (
  snapshot_id uuid primary key default gen_random_uuid(),
  encounter_id uuid references encounters(encounter_id),
  model_id uuid not null references queue_models(model_id),
  lambda double precision not null,
  mu double precision not null,
  sigma_s2 double precision,
  sigma_a2 double precision,
  rho double precision not null,
  lq double precision not null,
  wq double precision not null,
  l double precision not null,
  w double precision not null,
  cv double precision,
  ca_squared double precision,
  cs_squared double precision,
  computed_at_utc timestamptz default now()
);
