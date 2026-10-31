import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Delivery"))
from player_qa_preferences import DELETE, restoration_plan


class PlayerQaPreferencesTests(unittest.TestCase):
    def test_resolution_and_window_position_are_restored_with_their_registry_types(self):
        before = {"Screenmanager Resolution Width_h1": (1920, 4),
                  "Screenmanager Window Position X_h2": (20, 4), "Darklight.Music_h3": (b"data", 3)}
        after = {**before, "Screenmanager Resolution Width_h1": (1024, 4),
                 "Screenmanager Window Position X_h2": (100, 4)}
        changes, skipped = restoration_plan(before, after, after)
        self.assertEqual(changes, {name: before[name] for name in after if name.startswith("Screenmanager ")})
        self.assertEqual(skipped, [])

    def test_unrelated_human_setting_is_preserved(self):
        before = {"Darklight.Music_h3": (20, 4)}
        after = {"Darklight.Music_h3": (80, 4)}
        changes, skipped = restoration_plan(before, after, after)
        self.assertEqual(changes, {})
        self.assertEqual(skipped, ["Darklight.Music_h3"])

    def test_later_window_update_and_parallel_player_are_not_overwritten(self):
        key = "Screenmanager Resolution Width_h1"
        before, after = {key: (1920, 4)}, {key: (1024, 4)}
        self.assertEqual(restoration_plan(before, after, {key: (1600, 4)})[0], {})
        self.assertEqual(restoration_plan(before, after, after, concurrent_player=True)[0], {})

    def test_only_new_owned_values_are_removed(self):
        after = {"Screenmanager Window Position Y_h2": (50, 4), "Darklight.NewPreference": (1, 4)}
        changes, skipped = restoration_plan({}, after, after)
        self.assertIs(changes["Screenmanager Window Position Y_h2"], DELETE)
        self.assertEqual(skipped, ["Darklight.NewPreference"])


if __name__ == "__main__":
    unittest.main()
