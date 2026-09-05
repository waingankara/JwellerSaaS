create table if not exists making_charge_type (
    making_charge_type_id bigserial primary key,
    tenant_id bigint not null,

    making_charge_type_code varchar(30) not null,
    making_charge_type_name varchar(150) not null,

    display_order int not null default 0,
    remarks varchar(1000),

    is_active boolean not null default true,

    created_by bigint,
    created_date timestamptz,
    modified_by bigint,
    modified_date timestamptz,
    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid(),

    is_system boolean not null default false
);

create index if not exists ix_making_charge_type_tenant_id
    on making_charge_type (tenant_id);

create index if not exists ix_making_charge_type_name
    on making_charge_type (making_charge_type_name);

create index if not exists ix_making_charge_type_is_deleted
    on making_charge_type (is_deleted);

create unique index if not exists ux_making_charge_type_tenant_code_active
    on making_charge_type (
        tenant_id,
        upper(making_charge_type_code)
    )
    where is_deleted = false;

create unique index if not exists ux_making_charge_type_tenant_name_active
    on making_charge_type (
        tenant_id,
        upper(making_charge_type_name)
    )
    where is_deleted = false;