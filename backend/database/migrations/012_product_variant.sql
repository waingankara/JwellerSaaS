create table if not exists product_variant (
    product_variant_id bigserial primary key,
    tenant_id bigint not null,
    product_id bigint not null,

    sku varchar(100) not null,
    variant_name varchar(250),

    unit_id bigint,

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

create index if not exists ix_product_variant_tenant_id
    on product_variant (tenant_id);

create index if not exists ix_product_variant_product_id
    on product_variant (product_id);

create index if not exists ix_product_variant_unit_id
    on product_variant (unit_id);

create index if not exists ix_product_variant_is_deleted
    on product_variant (is_deleted);

create unique index if not exists ux_product_variant_tenant_sku_active
    on product_variant (tenant_id, upper(sku))
    where is_deleted = false;
