# CL_GUI_ALV_GRID

**Categoria:** Classe/Interface  
**Soluções:** 1  

[← Índice por elemento](../../indices/por_elemento.md)

---
<a id="s04122"></a>
### S04122

**Padrão:** [Somente código removido](../../padroes/P12_so_exclusao.md)  
**Ocorrências:** 1  
**Removido:** `CL_GUI_ALV_GRID` (Classe/Interface)  

**Problema (código original)**

```abap
    CREATE OBJECT v_handler3.
    SET HANDLER:
    v_handler3->handle_data_changed3  FOR v_grid3,
    v_handler3->handle_toolbar3       FOR v_grid3,
    v_handler3->handle_user_command3  FOR v_grid3,
    v_handler3->handle_hotspot_click3 FOR v_grid3.

    CALL METHOD v_grid3->register_edit_event
      EXPORTING
        i_event_id = cl_gui_alv_grid=>mc_evt_enter.

    CALL METHOD v_grid3->register_edit_event
      EXPORTING
        i_event_id = cl_gui_alv_grid=>mc_evt_modified.

    CALL METHOD v_grid3->set_frontend_layout
      EXPORTING
        is_layout = wa_layout.
```

**Solução (código novo)**

```abap
* (vazio)
```

---
