"""Plan reversible changes owned by a standalone QA run, without touching game settings."""
DELETE = object()


def qa_owned(name):
    return (name.startswith("Screenmanager ") or name.startswith("unity.player_session")
            or name.startswith("unity_connect.") and "session" in name)


def restoration_plan(before, after, current, concurrent_player=False):
    changes = sorted(name for name in before.keys() | after.keys()
                     if before.get(name, DELETE) != after.get(name, DELETE))
    operations, skipped = {}, []
    for name in changes:
        if (not qa_owned(name) or concurrent_player
                or current.get(name, DELETE) != after.get(name, DELETE)):
            skipped.append(name)
            continue
        operations[name] = before.get(name, DELETE)
    return operations, skipped
