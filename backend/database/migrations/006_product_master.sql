create table if not exists product (
    product_id bigserial primary key,
    tenant_id bigint not null,
    subcategory_id bigint not null,

    product_code varchar(50) not null,
    product_name varchar(250) not null,

    description varchar(2000),

    product_type_id bigint,
    brand_id bigint,
    collection_id bigint,

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

create index if not exists ix_product_tenant_id
    on product (tenant_id);

create index if not exists ix_product_subcategory_id
    on product (subcategory_id);

create index if not exists ix_product_product_type_id
    on product (product_type_id);

create index if not exists ix_product_brand_id
    on product (brand_id);

create index if not exists ix_product_collection_id
    on product (collection_id);

create index if not exists ix_product_product_name
    on product (product_name);

create index if not exists ix_product_is_deleted
    on product (is_deleted);

create index if not exists ix_product_tenant_subcategory
    on product (tenant_id, subcategory_id);

create unique index if not exists ux_product_tenant_code_active
    on product (tenant_id, upper(product_code))
    where is_deleted = false;

create unique index if not exists ux_product_tenant_name_active
    on product (tenant_id, upper(product_name))
    where is_deleted = false;
