-- +goose Up
-- +goose StatementBegin
CREATE SCHEMA IF NOT EXISTS issueneter;

CREATE TABLE IF NOT EXISTS issueneter.scans (
    id SERIAL NOT NULL
        CONSTRAINT scan_pkey PRIMARY KEY,
    scan_type text NOT NULL,
    owner text NOT NULL,
    repo text NULL,
    chat_id bigint NOT NULL,
    created TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    filters JSON NOT NULL
);

-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin

DROP TABLE issueneter.scans;
DROP SCHEMA issueneter;

-- +goose StatementEnd