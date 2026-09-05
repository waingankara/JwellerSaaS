create table if not exists stone_type (
    stone_type_id bigserial primary key,
    tenant_id bigint not null,

    stone_type_code varchar(30) not null,
    stone_type_name varchar(150) not null,

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

create index if not exists ix_stone_type_tenant_id
    on stone_type (tenant_id);

create index if not exists ix_stone_type_name
    on stone_type (stone_type_name);

create index if not exists ix_stone_type_is_deleted
    on stone_type (is_deleted);

create unique index if not exists ux_stone_type_tenant_code_active
    on stone_type (tenant_id, upper(stone_type_code))
    where is_deleted = false;

create unique index if not exists ux_stone_type_tenant_name_active
    on stone_type (tenant_id, upper(stone_type_name))
    where is_deleted = false;


create table if not exists stone_shape (
    stone_shape_id bigserial primary key,
    tenant_id bigint not null,

    stone_shape_code varchar(30) not null,
    stone_shape_name varchar(150) not null,

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

create index if not exists ix_stone_shape_tenant_id
    on stone_shape (tenant_id);

create index if not exists ix_stone_shape_name
    on stone_shape (stone_shape_name);

create index if not exists ix_stone_shape_is_deleted
    on stone_shape (is_deleted);

create unique index if not exists ux_stone_shape_tenant_code_active
    on stone_shape (tenant_id, upper(stone_shape_code))
    where is_deleted = false;

create unique index if not exists ux_stone_shape_tenant_name_active
    on stone_shape (tenant_id, upper(stone_shape_name))
    where is_deleted = false;


create table if not exists stone_color (
    stone_color_id bigserial primary key,
    tenant_id bigint not null,

    stone_color_code varchar(30) not null,
    stone_color_name varchar(150) not null,

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

create index if not exists ix_stone_color_tenant_id
    on stone_color (tenant_id);

create index if not exists ix_stone_color_name
    on stone_color (stone_color_name);

create index if not exists ix_stone_color_is_deleted
    on stone_color (is_deleted);

create unique index if not exists ux_stone_color_tenant_code_active
    on stone_color (tenant_id, upper(stone_color_code))
    where is_deleted = false;

create unique index if not exists ux_stone_color_tenant_name_active
    on stone_color (tenant_id, upper(stone_color_name))
    where is_deleted = false;


create table if not exists stone_clarity (
    stone_clarity_id bigserial primary key,
    tenant_id bigint not null,

    stone_clarity_code varchar(30) not null,
    stone_clarity_name varchar(150) not null,

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

create index if not exists ix_stone_clarity_tenant_id
    on stone_clarity (tenant_id);

create index if not exists ix_stone_clarity_name
    on stone_clarity (stone_clarity_name);

create index if not exists ix_stone_clarity_is_deleted
    on stone_clarity (is_deleted);

create unique index if not exists ux_stone_clarity_tenant_code_active
    on stone_clarity (tenant_id, upper(stone_clarity_code))
    where is_deleted = false;

create unique index if not exists ux_stone_clarity_tenant_name_active
    on stone_clarity (tenant_id, upper(stone_clarity_name))
    where is_deleted = false;