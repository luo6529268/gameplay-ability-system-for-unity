import { expect, test } from "@playwright/test";

test("loads the empty Canvas and connected status shell from the local origin", async ({ page }) => {
    await page.goto("/");

    await expect(page).toHaveTitle(/Dat Skill Flow/);
    await expect(page.getByRole("heading", { name: "Dat Skill Flow" })).toBeVisible();
    await expect(page.getByTestId("preview-canvas")).toBeVisible();
    await expect(page.getByTestId("server-status")).toHaveText("Connected to local server");
    expect(new URL(page.url()).hostname).toBe("127.0.0.1");
});
