create table if not exists metal (
    metal_id bigserial primary key,
    tenant_id bigint not null,

    metal_code varchar(30) not null,
    metal_name varchar(150) not null,

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

create index if not exists ix_metal_tenant_id
    on metal (tenant_id);

create index if not exists ix_metal_metal_name
    on metal (metal_name);

create index if not exists ix_metal_is_deleted
    on metal (is_deleted);

create unique index if not exists ux_metal_tenant_code_active
    on metal (tenant_id, upper(metal_code))
    where is_deleted = false;

create unique index if not exists ux_metal_tenant_name_active
    on metal (tenant_id, upper(metal_name))
    where is_deleted = false;


create table if not exists metal_purity (
    metal_purity_id bigserial primary key,
    tenant_id bigint not null,

    metal_id bigint not null,

    purity_code varchar(30) not null,
    purity_name varchar(150) not null,

    purity_percentage numeric(7,4) not null,

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

create index if not exists ix_metal_purity_tenant_id
    on metal_purity (tenant_id);

create index if not exists ix_metal_purity_metal_id
    on metal_purity (metal_id);

create index if not exists ix_metal_purity_purity_name
    on metal_purity (purity_name);

create index if not exists ix_metal_purity_is_deleted
    on metal_purity (is_deleted);

create unique index if not exists ux_metal_purity_tenant_metal_code_active
    on metal_purity (tenant_id, metal_id, upper(purity_code))
    where is_deleted = false;

create unique index if not exists ux_metal_purity_tenant_metal_name_active
    on metal_purity (tenant_id, metal_id, upper(purity_name))
    where is_deleted = false;