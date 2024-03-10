-- Enable uuid-ossp extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create table tgb_devices_sessions
CREATE TABLE tgb_devices_sessions (
    deviceId bigint NOT NULL,
    deviceTitle character varying(512) NOT NULL,
    "FormUri" character varying(512) NOT NULL,
    "QualifiedName" character varying(512) NOT NULL,
    CONSTRAINT PK_tgb_devices_sessions_1 PRIMARY KEY (deviceId)
);

-- Create table tgb_devices_sessions_data
CREATE TABLE tgb_devices_sessions_data (
    Id uuid DEFAULT uuid_generate_v4() NOT NULL,
    deviceId bigint NOT NULL,
    key character varying(512) NOT NULL,
    "value" text NOT NULL,
    "type" character varying(512) NOT NULL,
    CONSTRAINT PK_tgb_devices_session_data PRIMARY KEY (Id)
);

-- Add default constraint for Id column in tgb_devices_sessions_data
ALTER TABLE tgb_devices_sessions_data
    ALTER COLUMN Id SET DEFAULT uuid_generate_v4();
