create table if not exists price_component (
    price_component_id bigserial primary key,
    tenant_id bigint not null,

    price_component_code varchar(30) not null,
    price_component_name varchar(150) not null,

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

create index if not exists ix_price_component_tenant_id
    on price_component (tenant_id);

create index if not exists ix_price_component_name
    on price_component (price_component_name);

create index if not exists ix_price_component_is_deleted
    on price_component (is_deleted);

create unique index if not exists ux_price_component_tenant_code_active
    on price_component (
        tenant_id,
        upper(price_component_code)
    )
    where is_deleted = false;

create unique index if not exists ux_price_component_tenant_name_active
    on price_component (
        tenant_id,
        upper(price_component_name)
    )
    where is_deleted = false;