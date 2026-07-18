create table if not exists category (
    category_id bigserial primary key,
    tenant_id bigint not null,
    category_code varchar(20) not null,
    category_name varchar(200) not null,
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

create index if not exists ix_category_tenant_id on category (tenant_id);
create index if not exists ix_category_code on category (category_code);
create index if not exists ix_category_name on category (category_name);
create index if not exists ix_category_is_deleted on category (is_deleted);
create index if not exists ix_category_tenant_code on category (tenant_id, category_code);
create index if not exists ix_category_tenant_name on category (tenant_id, category_name);
create unique index if not exists ux_category_tenant_code_active on category (tenant_id, upper(category_code)) where is_deleted = false;
create unique index if not exists ux_category_tenant_name_active on category (tenant_id, upper(category_name)) where is_deleted = false;

insert into permission (code, name, resource, action, created_by, created_date) values
('CATEGORY_VIEW', 'View Category', 'category', 'view', 1, now()),
('CATEGORY_CREATE', 'Create Category', 'category', 'create', 1, now()),
('CATEGORY_UPDATE', 'Update Category', 'category', 'update', 1, now()),
('CATEGORY_DELETE', 'Delete Category', 'category', 'delete', 1, now()),
('CATEGORY_RESTORE', 'Restore Category', 'category', 'restore', 1, now()),
('CATEGORY_SEARCH', 'Search Category', 'category', 'search', 1, now()),
('CATEGORY_DROPDOWN', 'Category Dropdown', 'category', 'dropdown', 1, now())
on conflict (code) do nothing;
