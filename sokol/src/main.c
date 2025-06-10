#define SOKOL_AUDIO_IMPL
#define HANDMADE_MATH_IMPLEMENTATION
#define HANDMADE_MATH_NO_SSE
#define SOKOL_GL_IMPL
#define SOKOL_TIME_IMPL
#include "vendor/tinyfiledialogs/tinyfiledialogs.h"
#include "sokol_audio.h"
#include "sokol_app.h"
#include "sokol_gfx.h"
#include "sokol_log.h"
#include "sokol_glue.h"
#include "cimgui.h"
#include "sokol_imgui.h"
#include "sokol_gl.h"
#include <stdbool.h>
#include <stdio.h>
#include "lib/ephemeris.h"
#include "sokol_time.h"
#include "lib/sound.h"
#include "lib/utils.h"


#define PATH_SIZE 256
#define PLOT_SIZE 1000

static struct {
    float rx, ry;
    char selected_file[PATH_SIZE];
    char destination_file[PATH_SIZE];
    char* error;
    bool show_error;
    ephemeris_context_t ctx;
    sg_pass_action pass_action;
    sg_pipeline pip;
    sg_bindings bind;
} state;


static void init(void) {

    stm_setup();

    sg_setup(&(sg_desc){
       .environment = sglue_environment(),
       .logger.func = slog_func,
    });

    simgui_setup(&(simgui_desc_t){
        .logger.func = slog_func,
    });

    sgl_setup(&(sgl_desc_t){
        .logger.func = slog_func,
    });

    state.pass_action = (sg_pass_action){
        .colors[0] = { .load_action = SG_LOADACTION_CLEAR, .clear_value = { 0.1f, 0.1f, 0.1f, 1.0f } },
    };
    strcpy(state.selected_file, "/Users/dimas/moon.csv");
    strcpy(state.destination_file, "/Users/dimas/moon.bin");
    sound_setup();
}

void show_err(char *err) {
    state.show_error = true;
    state.error = err;

}

bool parse(ephemeris_context_t *ctx) {
    close_ephemeris(ctx);
    if (eph_parse_csv_file(state.selected_file, state.destination_file) != 0) {
        show_err("Failed to convert CSV file");
        return false;
    }
    if (load_ephemeris(state.destination_file, ctx) != 0) {
        show_err("Failed to load binary file");
        return false;
    }
    return true;
}

void dispaly_ephemeris() {
    igText("Total: %d", state.ctx.header.record_count);
    float alt[PLOT_SIZE];
    float azh[PLOT_SIZE];
    float dist[PLOT_SIZE];
    float phase[PLOT_SIZE];
    for (int i = 0; i  < PLOT_SIZE; ++i) {
        alt[i] = state.ctx.records[state.ctx.header.record_count - 1].altitude_deg;
        azh[i] = state.ctx.records[state.ctx.header.record_count - 1].azimuth_deg;
        dist[i] = state.ctx.records[state.ctx.header.record_count - 1].distance_km;
        phase[i] = state.ctx.records[state.ctx.header.record_count - 1].phase;
    }

    for (int i = 0; i < state.ctx.header.record_count && i < PLOT_SIZE; ++i) {
        ephemeris_record_t rec = state.ctx.records[i];
        alt[i] = rec.altitude_deg;
        azh[i] = rec.azimuth_deg;
        dist[i] = rec.distance_km;
        phase[i] =rec.phase;
    }

    igPlotLinesEx("Altitude", alt, PLOT_SIZE, 0, "Altitude", -180, 180, (ImVec2){.x = 1000, .y = 500}, sizeof(float));
    igPlotLinesEx("Azimuth", azh, PLOT_SIZE, 0, "Azimuth", 0, 360, (ImVec2){.x = 1000, .y = 500}, sizeof(float));
    igPlotLinesEx("Dist", dist, PLOT_SIZE, 0, "Dist", 0, 420000, (ImVec2){.x = 1000, .y = 500}, sizeof(float));
    igPlotLinesEx("Phase", phase, PLOT_SIZE, 0, "Phase", 0, 1, (ImVec2){.x = 1000, .y = 500}, sizeof(float));
}

static void frame(void) {

    simgui_new_frame(&(simgui_frame_desc_t){
        .width = sapp_width(),
        .height = sapp_height(),
        .delta_time = sapp_frame_duration(),
        .dpi_scale = sapp_dpi_scale(),
    });

    /*=== UI CODE STARTS HERE ===*/

    igSetNextWindowPos((ImVec2){10,10}, ImGuiCond_Once);
    igSetNextWindowSize((ImVec2){sapp_width() - 20, sapp_height()}, ImGuiCond_Once);


    if (igBegin("Uriel", 0,ImGuiWindowFlags_None)) {
        if (state.show_error) {
            igPushStyleColor(ImGuiCol_Text, IM_COL32(255,0,0,255));
            igText(state.error);
            igPopStyleColor();
        }


        igInputText("Ephemeries CSV", state.selected_file, 256, ImGuiInputFlags_None);

        if(igButton("Pick")) {
            char const * filterPatterns[1] = { "*.csv" };

            char const * selection = tinyfd_openFileDialog(
                "Select CSV",    // dialog title
                "",                     // default path
                1,                      // number of filter patterns
                filterPatterns,         // filter patterns
                "CSV",          // filter description
                0                       // allow multiple selection (0=no, 1=yes)
            );

            if (selection) {
                strncpy(state.selected_file, selection, sizeof(state.selected_file) - 1);
                state.selected_file[sizeof(state.selected_file) - 1] = '\0';
                replace_extension(state.selected_file, ".bin", state.destination_file, PATH_SIZE);
            }
        }

        igInputText("Destination BIN", state.destination_file, PATH_SIZE, ImGuiInputFlags_None);
        if (igButton("Change")) {
            char const * filterPatterns[1] = { "*.bin" };

            char const * selection = tinyfd_saveFileDialog(
                "Select CSV",
                state.destination_file,
                1,
                filterPatterns,
                "bin"
            );
            if (selection) {
                strncpy(state.destination_file, selection, sizeof(state.destination_file) - 1);
                state.destination_file[sizeof(state.destination_file) - 1] = '\0';
            }
        }

        if (strlen(state.selected_file) > 0) {
            if(igButton("Parse")) {
                parse(&state.ctx);
            }
        }

        if (state.ctx.header.record_count > 0) {
            dispaly_ephemeris();
        }

    }
    igEnd();

    /*=== UI CODE ENDS HERE ===*/

    sg_begin_pass(&(sg_pass){
        .action = state.pass_action,
        .swapchain = sglue_swapchain()
    });

    simgui_render();

    sg_end_pass();
    sg_commit();
}


static void cleanup(void) {
    sgl_shutdown();
    simgui_shutdown();

    sg_shutdown();
    saudio_shutdown();
}

static void event(const sapp_event* ev) {
    simgui_handle_event(ev);
}

sapp_desc sokol_main(const int argc, char* argv[]) {
    (void)argc;
    (void)argv;
    return (sapp_desc){
        .init_cb = init,
        .frame_cb = frame,
        .cleanup_cb = cleanup,
        .event_cb = event,
        .window_title = "Uriel",
        .width = 2500,
        .height = 1200,
        .icon.sokol_default = true,
        .logger.func = slog_func
    };
}