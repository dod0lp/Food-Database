RAISERROR('>>> Running 00_init_config.sql', 0, 1) WITH NOWAIT;
:r $(WORK_DIR)/init-scripts/00_init_config.sql
RAISERROR('>>> Finished 00_init_config.sql', 0, 1) WITH NOWAIT;

RAISERROR('>>> Running 01_init_create_db.sql', 0, 1) WITH NOWAIT;
:r $(WORK_DIR)/init-scripts/01_init_create_db.sql
RAISERROR('>>> Finished 01_init_create_db.sql', 0, 1) WITH NOWAIT;

RAISERROR('>>> Running 02_init_create_user.sql', 0, 1) WITH NOWAIT;
:r $(WORK_DIR)/init-scripts/02_init_create_user.sql
RAISERROR('>>> Finished 02_init_create_user.sql', 0, 1) WITH NOWAIT;

RAISERROR('>>> Running 03_init_create_table.sql', 0, 1) WITH NOWAIT;
:r $(WORK_DIR)/init-scripts/03_init_create_table.sql
RAISERROR('>>> Finished 03_init_create_table.sql', 0, 1) WITH NOWAIT;