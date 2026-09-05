-- ============================================================
-- Product Supporting Masters
-- ============================================================

-- ============================================================
-- 1. Product Type
-- ============================================================

create table if not exists product_type (
    product_type_id bigserial primary key,
    tenant_id bigint not null,

    product_type_code varchar(30) not null,
    product_type_name varchar(150) not null,

    display_order int not null default 0,
    remarks varchar(500),

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

create index if not exists ix_product_type_tenant_id
    on product_type (tenant_id);

create index if not exists ix_product_type_code
    on product_type (product_type_code);

create index if not exists ix_product_type_name
    on product_type (product_type_name);

create index if not exists ix_product_type_is_deleted
    on product_type (is_deleted);

create unique index if not exists ux_product_type_tenant_code_active
    on product_type (tenant_id, upper(product_type_code))
    where is_deleted = false;

create unique index if not exists ux_product_type_tenant_name_active
    on product_type (tenant_id, upper(product_type_name))
    where is_deleted = false;


-- ============================================================
-- 2. Brand
-- ============================================================

create table if not exists brand (
    brand_id bigserial primary key,
    tenant_id bigint not null,

    brand_code varchar(30) not null,
    brand_name varchar(150) not null,

    display_order int not null default 0,
    remarks varchar(500),

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

create index if not exists ix_brand_tenant_id
    on brand (tenant_id);

create index if not exists ix_brand_code
    on brand (brand_code);

create index if not exists ix_brand_name
    on brand (brand_name);

create index if not exists ix_brand_is_deleted
    on brand (is_deleted);

create unique index if not exists ux_brand_tenant_code_active
    on brand (tenant_id, upper(brand_code))
    where is_deleted = false;

create unique index if not exists ux_brand_tenant_name_active
    on brand (tenant_id, upper(brand_name))
    where is_deleted = false;


-- ============================================================
-- 3. Collection
-- ============================================================

create table if not exists collection (
    collection_id bigserial primary key,
    tenant_id bigint not null,

    collection_code varchar(30) not null,
    collection_name varchar(150) not null,

    display_order int not null default 0,
    remarks varchar(500),

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

create index if not exists ix_collection_tenant_id
    on collection (tenant_id);

create index if not exists ix_collection_code
    on collection (collection_code);

create index if not exists ix_collection_name
    on collection (collection_name);

create index if not exists ix_collection_is_deleted
    on collection (is_deleted);

create unique index if not exists ux_collection_tenant_code_active
    on collection (tenant_id, upper(collection_code))
    where is_deleted = false;

create unique index if not exists ux_collection_tenant_name_active
    on collection (tenant_id, upper(collection_name))
    where is_deleted = false;


-- ============================================================
-- 4. Unit Of Measure
-- ============================================================

create table if not exists unit_of_measure (
    unit_id bigserial primary key,
    tenant_id bigint not null,

    unit_code varchar(20) not null,
    unit_name varchar(100) not null,

    decimal_places int not null default 3,

    display_order int not null default 0,
    remarks varchar(500),

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

create index if not exists ix_unit_of_measure_tenant_id
    on unit_of_measure (tenant_id);

create index if not exists ix_unit_of_measure_code
    on unit_of_measure (unit_code);

create index if not exists ix_unit_of_measure_name
    on unit_of_measure (unit_name);

create index if not exists ix_unit_of_measure_is_deleted
    on unit_of_measure (is_deleted);

create unique index if not exists ux_unit_of_measure_tenant_code_active
    on unit_of_measure (tenant_id, upper(unit_code))
    where is_deleted = false;

create unique index if not exists ux_unit_of_measure_tenant_name_active
    on unit_of_measure (tenant_id, upper(unit_name))
    where is_deleted = false;
