<template>
	<h1>Add Broker</h1>
	<div class="form-group">
		<label>
			Name:
			<input ref="displayName" type="text" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Add</button>
	<router-link to="/" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';

@Options({
	name: 'AddBroker',
})
export default class AddBroker extends Vue {
	displayNameInput: HTMLInputElement | undefined;

	mounted() {
		this.displayNameInput = this.$refs.displayName as HTMLInputElement;
	}

	async onclick() {
		if (!this.displayNameInput) {
			console.error(
				`invalid setup (displayNameInput: ${this.displayNameInput}`
			);
			return;
		}
		const result = await Backend.tryFetch(
			Backend.state().broker.brokerCreate({
				displayName: this.displayNameInput.value,
			})
		);
		if (result?.ok) {
			await router.push('/');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
