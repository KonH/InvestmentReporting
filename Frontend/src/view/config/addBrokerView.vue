<template>
	<h1>Add Broker</h1>
	<div class="form-group">
		<label>
			Name:
			<input ref="displayName" type="text" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Add</button>
	<router-link to="/config" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Ref } from 'vue-property-decorator';
import { Action } from 'vuex-class';

@Options({
	name: 'AddBroker',
})
export default class AddBroker extends Vue {
	@Ref('displayName')
	displayNameInput!: HTMLInputElement;

	@Action('fetchState')
	fetchState!: () => void;

	async onclick() {
		const result = await Backend.tryFetch(
			Backend.state().broker.brokerCreate({
				displayName: this.displayNameInput.value,
			})
		);
		if (result.ok) {
			this.fetchState();
			await router.push('/config');
		} else {
			alert(`Failed: ${result.statusText}`);
		}
	}
}
</script>
